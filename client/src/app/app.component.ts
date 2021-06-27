import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Social App';
  users: any;

  constructor(private http: HttpClient, private accountService: AccountService) {}

  ngOnInit(): void {
    this.getAppUsers();
    this.setCurrentUser();
  }

  setCurrentUser(){
    const user: User = JSON.parse(localStorage.getItem('user') as any);
    this.accountService.setCurrentUser(user);
  }
  
  getAppUsers(){
    this.http.get('https://localhost:5001/api/AppUsers').subscribe(response => {
      this.users = response;
    }, err => {
      console.log(err)
    });
  }

}
