import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'client';
  users: any;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
      this.getUsers();
  }

  getUsers(){
    this.http.get('https://localhost:5001/api/users').subscribe(
      response => { this.users = response },
      (error: any) => { console.log(error) });
  }
}
