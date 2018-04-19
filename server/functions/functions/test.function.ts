import { Request, Response } from 'firebase-functions';

export class TestFunction {
    public async Test(req: Request, res: Response) {
        res.send('Sup dawg.');
    }
}