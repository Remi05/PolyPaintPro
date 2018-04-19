import * as storage from '@google-cloud/storage';
import { database } from 'firebase-admin';
import * as admin from 'firebase-admin';
import { CloudFunction, Event } from 'firebase-functions';
import * as functions from 'firebase-functions';
import { DeltaSnapshot } from 'firebase-functions/lib/providers/database';
import { ObjectMetadata } from 'firebase-functions/lib/providers/storage';

import { AnnotationService } from '../services/annotation.service';
import { HasTriggers } from './hasTrigger';

export class TagsTrigger implements HasTriggers {

    private static readonly allowedFolders = new Set(['images']);

    constructor(private database: database.Database, private storageClient: admin.storage.Storage, private annotationService: AnnotationService) { }

    public async generateTags(event: Event<ObjectMetadata>) {
        const data = event.data;
        const filePath = this.extractImagePath(data.name);
        const fileName = this.extractImageName(data.name);
        console.log(`Received event ${event.eventType} for file ${fileName}`);

        if (!TagsTrigger.allowedFolders.has(filePath)) {
            console.log(`Ignoring folder ${filePath}`);
            return;
        }

        console.log('Getting image url...');
        const url = await this.storageClient.bucket(data.bucket).file(data.name).getSignedUrl({ action: 'read', expires: '03-09-2491' });

        console.log(`Got the url! ${url[0]}`);
        const tags = await this.annotationService.annotate(url[0], 10);
        console.log(`Got the tags ${tags.join(', ')}!`)
        this.database.ref('drawingInfo').child(fileName).child('tags').set(tags);
    }

    private extractImageName(path: string) {
        // We split the path to get the file name (ex.: path/to/image.png => image.png)
        const splittedPath = path.split('/');
        const nameWithExtension = splittedPath[splittedPath.length - 1];

        // We split the file name to remove the extension (ex.: image.png => image)
        const splittedName = nameWithExtension.split('.');
        return splittedName[0];
    }

    private extractImagePath(path: string) {
        // We split the path to get the file name (ex.: path/to/image.png => image.png)
        const splittedPath = path.split('/');
        splittedPath.pop();

        // If the number of parts is 1, the file is at the root. If the number of parts is zero, we don't have a name.
        return splittedPath.join('/');
    }

    getTriggers(): CloudFunction<ObjectMetadata | DeltaSnapshot> {
        return functions.storage.object().onChange(event => this.generateTags(event));
    }
}