import * as storage from '@google-cloud/storage';
import * as admin from 'firebase-admin';
import * as functions from 'firebase-functions';

import { firebaseConfig } from './configs/firebaseConfig';
import { TagsTrigger } from './functions/tags.trigger';
import { TestFunction } from './functions/test.function';
import { AchievementsTrigger } from './functions/achievements.trigger';
import { AnnotationService } from './services/annotation.service';
import { serviceAccount } from './configs/serviceAccount';

// Services
const app = admin.initializeApp(firebaseConfig);
const annotationService = new AnnotationService();
const database = app.database();
const storageClient = app.storage();

// Functions        
const testFunction = new TestFunction();

// Triggers
const tagsTrigger = new TagsTrigger(database, storageClient, annotationService);
const achievementsTrigger = new AchievementsTrigger(database);

// Export function
export const achievements = achievementsTrigger.getTriggers();
export const tagsGenerator = tagsTrigger.getTriggers();
export const test = functions.https.onRequest((req, res) => testFunction.Test(req, res));