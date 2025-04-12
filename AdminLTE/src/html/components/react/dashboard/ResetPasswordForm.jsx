import { useState } from 'react'
import NotificationComponent from '@components/react/NotificationComponent';

const ResetPasswordForm = () => {
  const [oldPassword, setOldPassword] = useState('');
  const [password, setPassword] = useState('');
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
  const handleReset = async (event) => {
    event.preventDefault();
    if (cooldown) return;
    setIsLoading(true);
    setCooldown(true);
    try {
      const batch = {
        oldPassword: oldPassword,
        password: password
      }

      const response = await fetch('https://localhost:5033/users/reset-password', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(batch),
        credentials: 'include'
      });

      if (!response.ok) {
        if (response.status === 401) {
          showNotification("Invalid credentials. Please try again.", "error");
        } else {
          showNotification("An error occurred. Please try again later.", "error");
        }
      } else {
        showNotification("User Password Changed", "success")
      }
    } catch (error) {
      showNotification("Error during authentication.", "error");
    } finally {
      setIsLoading(false);
      setTimeout(() => setCooldown(false), 1500);
    }
  }

  return (
    <>
      <form onSubmit={handleReset} className="mt-3 mb-3 me-5 ms-5 ">
        <div className="row align-items-end">
          <div className="form-group col-12 col-md-4">
            <label className='me-3' htmlFor="oldPassword">Old Password: </label>
            <input
              name='oldPassword'
              id='oldPassword'
              type='password'
              value={oldPassword}
              className='form-control'
              onChange={(e) => setOldPassword(e.target.value)}
              placeholder=''
              required
            />
          </div>
          <div className="form-group col-12 col-md-4">
            <label className='me-3' htmlFor="password">New Password: </label>
            <input
              name='password'
              id='password'
              type='password'
              value={password}
              className='form-control'
              onChange={(e) => setPassword(e.target.value)}
              placeholder=''
              required
            />
          </div>
          <div className="form-group col-12 col-md-4 d-flex mt-2 justify-content-center">
            <button type="submit" className="btn btn-primary"
              disabled={isLoading}
            >
              {isLoading ? "Reseting..." : "Reset"}
            </button>
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