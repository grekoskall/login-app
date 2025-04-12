import { useState } from 'react';
import NotificationComponent from '@components/react/NotificationComponent';

const LoginForm = () => {
  const [loginEmail, setLoginEmail] = useState('');
  const [loginPassword, setLoginPassword] = useState('');
  const [notificationMessage, setNotificationMessage] = useState("");
  const [notificationCount, setNotificationCount] = useState(0);
  const [notificationType, setNotificationType] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [cooldown, setCooldown] = useState(false);

  const showNotification = (message, type) => {
    setNotificationMessage(message);
    setNotificationType(type);
    setNotificationCount((prevKey) => prevKey + 1);
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (cooldown) return;
    setIsLoading(true);
    setCooldown(true);
    const loginData = {
      loginEmail,
      loginPassword,
    };

    try {
      const response = await fetch('https://localhost:5033/v1/auth/authenticate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(loginData),
      });

      setTimeout(() => { }, 3500);
      if (!response.ok) {
        if (response.status === 401) {
          showNotification("Invalid credentials. Please try again.", "error");
        } else {
          showNotification("An error occurred. Please try again later.", "error");
        }
      } else {
        // Redirect for 2fa on successful login.
        window.location.href = '/v1/auth/verify-2fa.html';
      }
    } catch (error) {
      showNotification("Error during authentication.", "error");
    } finally {
      setIsLoading(false);
      setTimeout(() => setCooldown(false), 1500);
    }
  };

  return (
    <div className='login-box'>
      <div className='card card-outline card-primary'>
        <div className='card-header mt-2'>
          <p className='login-box-msg p-0 mb-2 fs-4'>Login</p>
        </div>
        <div className='card-body login-card-body'>
          <form onSubmit={handleSubmit} >
            <div className='input-group mb-4'>
              <div className='form-floating'>
                <input
                  name='loginEmail'
                  id='loginEmail'
                  type='email'
                  className='form-control'
                  value={loginEmail}
                  onChange={(e) => setLoginEmail(e.target.value)}
                  placeholder=''
                  required
                />
                <label htmlFor='loginEmail'>Email</label>
              </div>
              <div className='input-group-text'>
                <span className='bi bi-envelope'></span>
              </div>
            </div>
            <div className='input-group mb-1'>
              <div className='form-floating'>
                <input
                  id='loginPassword'
                  type='password'
                  className='form-control'
                  name='loginPassword'
                  value={loginPassword}
                  onChange={(e) => setLoginPassword(e.target.value)}
                  placeholder=''
                  required
                />
                <label htmlFor='loginPassword'>Password</label>
              </div>
              <div className='input-group-text'>
                <span className='bi bi-lock-fill'></span>
              </div>
            </div>

            <p className='mb-4 text-end'>
              <a href='/v1/login/forgot-password.html'>Forgot Password?</a>
            </p>
            <div className='row justify-content-center mb-4'>
              <div className='col-6'>
                <div className='d-grid gap-2'>
                  <button type="submit" className="btn btn-primary" disabled={isLoading}>
                    Sign In
                  </button>
                </div>
              </div>
            </div>
          </form>
        </div>
      </div>
      {/* Notification Component */}
      {notificationMessage &&
        <NotificationComponent
          client:load
          message={notificationMessage}
          type={notificationType}
          key={notificationCount} />}
    </div>

  );
};

export default LoginForm;
