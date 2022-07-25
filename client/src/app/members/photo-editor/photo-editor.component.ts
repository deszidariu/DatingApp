import { JsonPipe } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs';
import { User } from 'src/app/_models/user';
import { Member } from 'src/app/_modules/member';
import { Photo } from 'src/app/_modules/photo';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() member: Member;
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseurl = environment.apiUrl;
  usr: User;

  constructor(private memberService: MembersService, private accountService: AccountService) { 
    this.accountService.currentuser$.pipe(take(1)).subscribe(user => this.usr = user);
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e: any){
    this.hasBaseDropZoneOver = e;
  }

  setMainPhoto(photo: Photo){
    this.memberService.setMainPhoto(photo.id).subscribe(() => {
      this.usr.photoUrl = photo.url;
      this.accountService.settCurrentUser(this.usr);
      this.member.photoUrl = photo.url;
      this.member.photos.forEach(p =>{
        if(p.isMain) p.isMain = false;
        if(p.id == photo.id) p.isMain = true;
      })
    })
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseurl + "users/add-photo",
      authToken: "Bearer " + this.usr.token,
      isHTML5: true,
      allowedFileType: ["image"],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    }

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if(response){
        const photo = JSON.parse(response);
        this.member.photos.push(photo);
      }
    }
  }

  deletePhoto(photoId: number){
    this.memberService.deletePhoto(photoId).subscribe(() =>{
      this.member.photos = this.member.photos.filter(x => x.id !== photoId);
    })
  }
}