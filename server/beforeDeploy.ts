import * as admin from 'firebase-admin';
import { firebaseConfig } from './functions/configs/firebaseConfig';
import { achievementsDefinitions } from './achievementsDefinitions';

async function beforeDeploy() {
    console.log('Executing before deploy script.')
    const app = admin.initializeApp(firebaseConfig);
    const database = app.database();

    // Pushing achievements definitions
    await database.ref('achievements').child('definitions').set(achievementsDefinitions);
    process.exit();
}

beforeDeploy();