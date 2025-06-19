import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class CustomValidators {
  static passwordStrength(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value: string = control.value || '';
      if (!value) {
        return null;
      }
      const hasNumber = /[0-9]/.test(value);
      const hasSymbol = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]+/.test(value);
      const valid = value.length >= 8 && hasNumber && hasSymbol;
      return !valid ? { passwordStrength: true } : null;
    };
  }

  static bannedWords(bannedWords: string[]): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value: string = control.value || '';
      if (!value) {
        return null;
      }
      const isBanned = bannedWords.some(word => value.toLowerCase().includes(word.toLowerCase()));
      return isBanned ? { bannedWord: true } : null;
    };
  }

  static passwordMatch(passwordControlName: string, confirmPasswordControlName: string): ValidatorFn {
    return (formGroup: AbstractControl): ValidationErrors | null => {
      const passwordControl = formGroup.get(passwordControlName);
      const confirmPasswordControl = formGroup.get(confirmPasswordControlName);

      if (!passwordControl || !confirmPasswordControl || confirmPasswordControl.errors && !confirmPasswordControl.errors['mismatch']) {
        return null;
      }

      if (passwordControl.value !== confirmPasswordControl.value) {
        confirmPasswordControl.setErrors({ mismatch: true });
        return { mismatch: true };
      } else {
        confirmPasswordControl.setErrors(null); 
        return null;
      }
    };
  }
}