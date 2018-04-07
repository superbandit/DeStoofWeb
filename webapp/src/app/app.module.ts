import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { ChatComponent } from './chat/chat.component';
import { ChatService } from './Services/chatService';

@NgModule({
  declarations: [AppComponent, ChatComponent],
  imports: [BrowserModule, HttpClientModule],
  providers: [ChatService, HttpClientModule],
  bootstrap: [AppComponent]
})
export class AppModule { }
