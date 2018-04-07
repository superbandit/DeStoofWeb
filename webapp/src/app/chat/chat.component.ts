import { Component, OnInit } from '@angular/core';
import { HubConnection } from '@aspnet/signalr';
import { ChatMessage} from './chatMessage';
import { ChatService } from '../Services/chatService';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {

  private hubConnection: HubConnection;
  message = '';
  messages: ChatMessage[] = [];

  constructor(private _chatService: ChatService) { }

  ngOnInit() {
    this.hubConnection = new HubConnection('http://localhost:5000/chat');

    this.hubConnection
      .start()
      .then(() => console.log('Connection started!'))
      .catch(err => console.log('Error while establishing connection :('));

    this.hubConnection.on('Send', (receivedMessageString: string) => {
      const receivedMessage = JSON.parse(receivedMessageString);
      this.messages.push(receivedMessage);
    });
  }

  public connectDiscord(channel: string) {
    this._chatService.startDiscordConnection(channel).subscribe(
      data => {},
      error => console.error(error),
      () => console.log(`Connected to ${channel}`)
    );
  }
}
