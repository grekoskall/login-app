import { useState } from 'react'
import NotificationComponent from '@components/react/NotificationComponent';

const LoginInstructionsForm = () => {
  const basePath = '../../../../dist/assets/img/'
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
  const handleSend = async (event) => {
    event.preventDefault();
    if (cooldown) return;
    setIsLoading(true);
    setCooldown(true);
    try {
      const response = await fetch('https://localhost:5033/users/login-instructions', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include'
      });

      if (!response.ok) {
        if (response.status === 401) {
          showNotification("Invalid credentials. Please try again.", "error");
        } else {
          showNotification("An error occurred. Please try again later.", "error");
        }
      } else {
        const response = await fetch('https://localhost:5033/v1/auth/logout-user', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          credentials: 'include'
        });
        if (!response.ok) {
          showNotification("An error occured.", "error");
        } else {
          window.location.href = '/';
        }
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
      <div>
        <div className="d-flex">
          <button onClick={handleSend} className="btn btn-warning"
            disabled={isLoading}
          >
            Send Login Instructions
          </button>
        </div>
      </div>
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

export default LoginInstructionsForm;