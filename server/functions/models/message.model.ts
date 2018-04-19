export class MessageModel {
    constructor(public senderId: string, public senderName: string, public text: string, public timestamp: Date) { }
}