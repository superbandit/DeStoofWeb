import { Component, OnInit } from '@angular/core';
import { HubConnection } from '@aspnet/signalr';
import { ChatMessage} from './chatMessage';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {

  private hubConnection: HubConnection;
  message = '';
  messages: ChatMessage[] = [];

  constructor() { }

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
}
