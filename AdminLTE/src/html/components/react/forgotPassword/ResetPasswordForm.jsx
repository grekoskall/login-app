import { useState, useEffect } from 'react'
import NotificationComponent from '@components/react/NotificationComponent';

const ResetPasswordForm = () => {
  const [token, setToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [loading, setLoading] = useState(false)
  const [notificationMessage, setNotificationMessage] = useState("");
  const [notificationCount, setNotificationCount] = useState(0);
  const [notificationType, setNotificationType] = useState("");

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const tokenFromUrl = params.get('token');
    if (tokenFromUrl) {
      setToken(tokenFromUrl);
    }
  }, []);

  const showNotification = (message, type) => {
    setNotificationMessage(message);
    setNotificationType(type);
    setNotificationCount((prevKey) => prevKey + 1);
  };


  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      setLoading(true);
      const batch = {
        Token: token,
        NewPassword: newPassword
      }

      const res = await fetch(
        'https://localhost:5033/v1/auth/reset-password',
        {
          method: "POST",
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(batch)
        }
      );

      if (!res.ok) {
        showNotification("An error occurred. Please try again later.", "error");
      } else {
        showNotification("Password reset successfully.", "success");
      }
    } catch (ex) {
      showNotification("Error during password reset.", "error");
    } finally {
      setLoading(false);
    }
  }

  return (
    <>
      <form onSubmit={handleSubmit}>
        <div className='input-group mb-4'>
          <div className='form-floating'>
            <input
              id='password'
              type='password'
              className='form-control'
              value={newPassword}
              onChange={(e) => { setNewPassword(e.target.value) }}
              placeholder=''
            />
            <label htmlFor='password'>New Password</label>
          </div>
          <div className='input-group-text'>
            <span className='bi bi-code'></span>
          </div>
        </div>
        <div className='row justify-content-center mb-4'>
          <div className='col-6'>
            <div className='d-grid gap-2'>
              <button type='submit' className='btn btn-primary' disabled={loading}>
                {loading ? 'Resetting' : 'Reset'}
              </button>
            </div>
          </div>
        </div>
      </form>
      {/* Notification Component */}
      {notificationMessage &&
        <NotificationComponent
          client:load
          message={notificationMessage}
          type={notificationType}
          key={notificationCount} />}
    </>
  )
}


export default ResetPasswordForm;