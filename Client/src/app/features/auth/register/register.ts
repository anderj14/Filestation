import {Component, inject} from '@angular/core';
import {Authservice} from '../../../core/services/authservice';
import {Router} from '@angular/router';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';

@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private authService = inject(Authservice);
  private router = inject(Router);

  registerForm = new FormGroup({
    username: new FormControl('', [Validators.required]),
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required]),
  });


  errors: string[] = [];
  loading = false;

  onSubmit() {
    if (this.registerForm.invalid) return;
    this.loading = true;
    this.errors = [];

    this.authService.register(this.registerForm.value as any).subscribe({
      next: () => {
        this.router.navigateByUrl('/auth/login');
      },
      error: (err) => {
        this.errors = err?.error?.error || ['Error desconocido'];
        this.loading = false;
      }
    });
  }

}
