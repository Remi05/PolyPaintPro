import { database } from 'firebase-admin';
import { CloudFunction, Event } from 'firebase-functions';
import * as functions from 'firebase-functions';
import { DeltaSnapshot } from 'firebase-functions/lib/providers/database';
import { MessageModel } from '../models/message.model';
import { AchievementDefinitionModel } from '../models/achievementDefinition.model';
import { user } from 'firebase-functions/lib/providers/auth';


export class AchievementsTrigger {
    private readonly achievementsPath = 'achievements';
    private readonly definitionsPath = 'definitions';
    private readonly metricsPath = 'metrics';
    private readonly completedPath = 'completed';

    constructor(private database: database.Database) { }

    public async generateTags(event: Event<DeltaSnapshot>) { }

    public async messages(event: Event<DeltaSnapshot>) {
        const message = event.data.val() as MessageModel;
        const author = message.senderId;
        const achievementRef = this.database.ref(this.achievementsPath)
            .child(author)
            .child(this.metricsPath)
            .child('numberOfMessages');

        let count = (await achievementRef.once('value')).val() as number;
        achievementRef.set(count === undefined ? 1 : ++count);
    }

    public async drawings(event: Event<DeltaSnapshot>) {
        const drawings = event.data.val() as string;
        const author = event.data.adminRef.parent.key;
        console.log(`New drawing ${drawings} from user ${author}.`)

        const achievementRef = this.database.ref(this.achievementsPath)
            .child(author)
            .child(this.metricsPath)
            .child('numberOfDrawings');

        let count = (await achievementRef.once('value')).val() as number;
        achievementRef.set(count === undefined ? 1 : ++count);
    }

    public async achievements(event: Event<DeltaSnapshot>) {
        const author = event.data.adminRef.parent.key;
        if (!event.data.val()) { return; }

        console.log(`New achievement for user ${author}.`)

        const achievementRef = this.database.ref(this.achievementsPath)
            .child(author)
            .child(this.metricsPath)
            .child('numberOfAchievements');

        let count = (await achievementRef.once('value')).val() as number;
        achievementRef.set(count === undefined ? 1 : ++count);
    }

    private async updateAchievements(userId: string) {
        console.log(`Updating achievements for user ${userId}`);
        const achievements = (await this.database.ref(this.achievementsPath)
            .child(this.definitionsPath)
            .once('value')).val() as { [id: string]: AchievementDefinitionModel };

        const metrics = (await this.database.ref(this.achievementsPath)
            .child(userId)
            .child(this.metricsPath)
            .once('value')).val();

        const completedAchievements: { [achievementId: string]: boolean } = {};

        for (const id in achievements) {
            if (!achievements.hasOwnProperty(id)) continue;
            const achievement = achievements[id];

            if (metrics[achievement.metric] >= achievement.count) {
                completedAchievements[id] = true;
            }
        }

        console.log(`New achievements: ${Object.keys(completedAchievements).join(', ')}`);

        await this.database.ref(this.achievementsPath)
            .child(userId)
            .child(this.completedPath)
            .update(completedAchievements);
    }

    private async increment(metric: string, author: string) {
        const achievementRef = this.database.ref(this.achievementsPath)
            .child(author)
            .child(this.metricsPath)
            .child(metric);

        let count = (await achievementRef.once('value')).val() as number;
        achievementRef.set(count === undefined ? 1 : ++count);
    }

    public getTriggers(): CloudFunction<DeltaSnapshot>[] {
        return [
            functions.database.ref('messages/{conversationId}/{messageId}')
                              .onWrite(event => {
                                  if(!event.data.val()) return;
                                  this.increment('numberOfMessages', event.data.val().senderId);
                              }),

            functions.database.ref('users/{userId}/drawings')
                              .onWrite(event => {
                                   if(!event.data.val()) return;
                                   this.increment('numberOfDrawings' , event.data.adminRef.parent.key);                           
                              }),

            functions.database.ref('achievements/{userId}/completed')
                               .onWrite(event => {
                                   if(!event.data.val()) return;
                                   this.increment('numberOfAchievements', event.data.adminRef.parent.key);
                               }),

            functions.database.ref('users/{userId}/followers')
                              .onWrite(event => {
                                  if(!event.data.val()) return;
                                  this.increment('numberOfFollowers', event.data.adminRef.parent.key);
                              }),

            functions.database.ref('users/{userId}/following')
                              .onWrite(event => {
                                  if(!event.data.val()) return;
                                  this.increment('numberOfFollowings', event.data.adminRef.parent.key);
                              }),

            functions.database.ref('achievements/{userId}/metrics')
                              .onWrite(event => this.updateAchievements(event.data.adminRef.parent.key)),

            functions.database.ref('achievements/{userId}/metrics/{metric}')
                              .onWrite(event => this.updateAchievements(event.data.adminRef.parent.parent.key)),
        ];
    }
}