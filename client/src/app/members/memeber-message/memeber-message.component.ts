import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './memeber-message.component.html',
  styleUrls: ['./memeber-message.component.css']
})
export class MemeberMessageComponent implements OnInit {
  @ViewChild('messageForm') messageForm: NgForm;
  @Input() username: string;
  messageContent: string;
  loading = false;

  constructor(public messageService: MessageService) { }

  ngOnInit(): void {
  }

    sendMessage(){
      if(!this.username) return;
      this.loading = true;
      this.messageService.sendMessage(this.username, this.messageContent).then(() => {
        this.messageForm?.reset();
      }).finally(() => this.loading = false);
    }
}
