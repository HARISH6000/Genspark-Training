import { TestBed } from '@angular/core/testing';
import { AuthGuard } from './auth-guard';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';

describe('AuthGuard', () => {
  let guard: AuthGuard;
  let router: Router;
  let route: ActivatedRouteSnapshot;
  let state: RouterStateSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RouterTestingModule.withRoutes([])],
      providers: [AuthGuard]
    });

    guard = TestBed.inject(AuthGuard);
    router = TestBed.inject(Router);

    route = {} as ActivatedRouteSnapshot;
    state = { url: '/protected' } as RouterStateSnapshot;
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should allow activation if token exists', () => {
    localStorage.setItem('token', 'fake-token');
    const result = guard.canActivate(route, state);
    expect(result).toBeTrue();
  });

  it('should block activation and redirect if token is missing', () => {
    spyOn(router, 'navigate');
    localStorage.removeItem('token');

    const result = guard.canActivate(route, state);

    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['login']);
  });
});
