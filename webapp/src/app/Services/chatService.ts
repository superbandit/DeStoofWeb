import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class ChatService {
    constructor(private http: HttpClient) {
    }

    public startDiscordConnection(channel: string) {
        const body = '';
        return this.http.post(`http://localhost:5000/api/chat/connectDiscord/${channel}`, body);
    }
}
