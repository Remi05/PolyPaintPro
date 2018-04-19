import { AchievementDefinitionModel } from './functions/models/achievementDefinition.model';

export const achievementsDefinitions: { [id: string]: AchievementDefinitionModel } = {
    "messages-1": {
        name: "First base",
        message: "Wow! You sent your first message!",
        description: "Send your first message in the chat.",
        metric: "numberOfMessages",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Fchat-bronze.png?alt=media&token=8f410b6c-df0f-4597-b450-cc83131423db",
        count: 1
    },
    "messages-2": {
        name: "Smooth talker",
        message: "You are quite the smooth talker!",
        description: "Send fifty messages in the chat.",
        metric: "numberOfMessages",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Fchat-silver.png?alt=media&token=f05be385-53bb-4502-a779-3d65bac9fbd1",
        count: 25
    },
    "messages-3": {
        name: "Talkative",
        message: "Damn! You're the coolest kid in the school!",
        description: "Send one hundred messages in the chat.",
        metric: "numberOfMessages",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Fchat-gold.png?alt=media&token=992f286f-30d4-4509-b851-857cde8dd7c5",
        count: 100
    },
    "achievements-1": {
        name: "Maximum paradox",
        message: "You just got an achievement for getting an achievement, how paradoxal!",
        description: "Get one achievement.",
        metric: "numberOfAchievements",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Fachievements-2.png?alt=media&token=b1489316-6f05-49d9-9cd9-15fcaa6f4235",
        count: 1
    },
    "achievements-2": {
        name: "Overachiever",
        message: "You are a master, nothing can stop you.",
        description: "Get all the achievements (minus one, obviously).",
        metric: "numberOfAchievements",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Fachievements-2.png?alt=media&token=b1489316-6f05-49d9-9cd9-15fcaa6f4235",
        count: 17
    },
    "drawings-1": {
        name: "Apprentice",
        message: "This is your first drawing! WOW!",
        description: "Draw your first piece of art.",
        metric: "numberOfDrawings",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Fdrawings-1.png?alt=media&token=3ca81d54-bfcd-4ef8-9d01-a46aa30c2bd1",
        count: 1
    },
    "drawings-2": {
        name: "Addicted to drawing",
        message: "Be proud, this is your fifth drawing! Or be ashamed, like, for real, how much time do you spend on this app?",
        description: "Draw 25 pieces of art.",
        metric: "numberOfDrawings",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Fdrawings-2.png?alt=media&token=ba6c2886-9647-4fb3-b218-49f32d8acd8a",
        count: 25
    },
    "drawings-3": {
        name: "Real Bob Ross",
        message: "Seriously, are you Bob Ross?",
        description: "Draw 100 masterpieces.",
        metric: "numberOfDrawings",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Fdrawings-3.png?alt=media&token=408b966f-bf01-4bd5-99ed-3cf33596e24b",
        count: 100
    },
    "share-on-facebook": {
        name: "Facebook famous",
        message: "Wow, I bet your friends are going to like that.",
        description: "Share one drawing on facebook.",
        metric: "sharesOnFacebook",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Fshare-on-facebook.png?alt=media&token=aa59de1a-6412-4c71-9f9b-9bd66d1d2493",
        count: 1
    },
    "share-on-drive": {
        name: "S*%? up and drive",
        message: "Wow, you shared on Drive, that's something, I guess.",
        description: "Share one drawing on facebook.",
        metric: "sharesOnDrive",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Fshare-on-drive.png?alt=media&token=20a83382-4e00-4fd4-98ea-e4b3e4c26572",
        count: 1
    },
    "follow-1": {
        name: "Interested in people",
        message: "You followed your first person!",
        description: "Follow one person.",
        metric: "numberOfFollowings",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Ffollow-1.png?alt=media&token=dfb528d7-4f26-4cca-87df-932fb50ca5c9",
        count: 1
    },
    "follow-2": {
        name: "Genuinely interested in people",
        message: "“Friends are the best to turn to when you're having a rough day” - My boi Justin Bieber",
        description: "Follow 25 people.",
        metric: "numberOfFollowings",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Ffollow-2.png?alt=media&token=2b631c32-b037-4a84-80b2-0f90ed6a0759",
        count: 25
    },
    "follow-3": {
        name: "Creep",
        message: "But I'm a creep... I'm a weirdo-oo-oh-oo...",
        description: "Follow 100 people.",
        metric: "numberOfFollowings",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Ffollow-3.png?alt=media&token=5f38feb3-a483-4f9b-9053-bf4c5088e0b7",
        count: 100
    },
    "followers-1": {
        name: "A bit popular",
        message: "You have one follower.",
        description: "Have one follower.",
        metric: "numberOfFollowers",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Ffollowers-1.png?alt=media&token=7a3e619c-842f-40f5-a0e8-2b69197af80e",
        count: 1
    },
    "followers-2": {
        name: "A bit more popular",
        message: "You are a bit popular.",
        description: "Have 25 followers.",
        metric: "numberOfFollowers",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Ffollowers-2.png?alt=media&token=c11dbc5a-0f3d-41fe-a2fb-c2984e70bb19",
        count: 25
    },
    "followers-3": {
        name: "Really popular.",
        message: "You are really popular.",
        description: "Have 100 followers.",
        metric: "numberOfFollowers",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Ffollowers-3.png?alt=media&token=935c0e79-cce0-4d1e-8e83-a53ed2fa97f8",
        count: 100
    },
    "nsfw": {
        name: "Naughty",
        message: "Naughty naughty!",
        description: "Draw one not safe for work drawing.",
        metric: "numberOfNsfwDrawings",
        iconUri: "https://firebasestorage.googleapis.com/v0/b/polypaintpro.appspot.com/o/achievements%2Fnsfw.png?alt=media&token=e301f8b4-5154-4494-98d4-247bed963a71",
        count: 1
    }
};