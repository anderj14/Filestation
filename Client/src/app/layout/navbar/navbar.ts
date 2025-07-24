import {Component, inject, OnInit, signal} from '@angular/core';
import { Authservice } from '../../core/services/authservice';

@Component({
  selector: 'app-navbar',
  imports: [],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss'
})
export class Navbar implements OnInit {

  auth = inject(Authservice);
  showDropdown = signal(false);

  ngOnInit(): void {
    if (this.auth.isLoggedIn()) {
      this.auth.loadCurrentUserFromApi().subscribe();
    }
  }

  toggleDropdown() {
    this.showDropdown.update(value => !value);
  }

  logout() {
    this.auth.logout();
    this.showDropdown.set(false);
  }


}
