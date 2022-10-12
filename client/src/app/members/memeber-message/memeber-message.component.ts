import { NgForOf } from '@angular/common';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-memeber-message',
  templateUrl: './memeber-message.component.html',
  styleUrls: ['./memeber-message.component.css']
})
export class MemeberMessageComponent implements OnInit {
  @ViewChild('messageForm') messageForm: NgForm;
  @Input() messages: Message[];
  @Input() username: string;
  messageContent: string;

  constructor(private messageServ: MessageService) { }

  ngOnInit(): void {
  }

    sendMessage(){
      this.messageServ.sendMessage(this.username, this.messageContent).subscribe(message =>{
        this.messages.push(message);
        this.messageForm.reset();
      })
    }
}
