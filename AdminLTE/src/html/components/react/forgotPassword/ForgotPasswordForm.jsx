import { useState } from 'react'
import NotificationComponent from '@components/react/NotificationComponent';

const ForgotPasswordForm = () => {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false)
  const [notificationMessage, setNotificationMessage] = useState("");
  const [notificationCount, setNotificationCount] = useState(0);
  const [notificationType, setNotificationType] = useState("");

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
        email
      }

      const res = await fetch(
        'https://localhost:5033/v1/auth/forgot-password',
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
        showNotification("Email with reset details sent.", "success");
      }
    } catch (ex) {
      showNotification("Invalid password.", "error");
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
              id='email'
              type='email'
              className='form-control'
              value={email}
              onChange={(e) => { setEmail(e.target.value) }}
              placeholder=''
            />
            <label htmlFor='email'>Email</label>
          </div>
          <div className='input-group-text'>
            <span className='bi bi-envelope'></span>
          </div>
        </div>
        <div className='row justify-content-center mb-4'>
          <div className='col-6'>
            <div className='d-grid gap-2'>
              <button type='submit' className='btn btn-primary' disabled={loading}>
                {loading ? 'Sending' : 'Send'}
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


export default ForgotPasswordForm;