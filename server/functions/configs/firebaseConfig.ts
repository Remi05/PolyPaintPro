import * as admin from 'firebase-admin';
import { serviceAccount } from './serviceAccount';

export const firebaseConfig = {
    credential: admin.credential.cert(serviceAccount as any),
    storageBucket: 'gs://polypaintpro.appspot.com',
    databaseURL: 'https://polypaintpro.firebaseio.com'
}
