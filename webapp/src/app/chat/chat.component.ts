import { Component, OnInit } from '@angular/core';
import { HubConnection } from '@aspnet/signalr';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {

  private hubConnection: HubConnection;
  message = '';
  messages: string[] = [];

  constructor() { }

  ngOnInit() {
    this.hubConnection = new HubConnection('http://localhost:5000/chat');

    this.hubConnection
      .start()
      .then(() => console.log('Connection started!'))
      .catch(err => console.log('Error while establishing connection :('));

    this.hubConnection.on('Send', (receivedMessage: string) => {
      this.messages.push(receivedMessage);
    });
  }
}
