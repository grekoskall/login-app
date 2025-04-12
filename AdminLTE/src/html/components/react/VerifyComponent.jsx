import { useState } from 'react';
import NotificationComponent from '@components/react/NotificationComponent';


const VerifyForm = () => {
  const [verificationCode, setVerificationCode] = useState('');
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

  const handleVerify = async (event) => {
    event.preventDefault();
    if (cooldown) return;
    setIsLoading(true);
    setCooldown(true);

    const twoFactorRequest = {
      Enable: true,
      TwoFactorCode: verificationCode
    };

    try {
      const response = await fetch('https://localhost:5033/v1/auth/verify-2fa', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(twoFactorRequest),
        credentials: 'include'
      });

      setTimeout(() => { }, 3500);
      if (!response.ok) {
        if (response.status === 401) {
          showNotification("Invalid code.", "error");
        } else {
          showNotification("An error occurred. Please try again later.", "error");
        }
      } else {
        window.location.href = '/dashboard.html';
      }
    } catch (error) {
      showNotification("Error during verification.", "error");
    } finally {
      setIsLoading(false);
      setTimeout(() => setCooldown(false), 1500);
    }
  }

  return (
    <div className='login-box'>
      <div className='card card-outline card-primary'>
        <div className='card-header mt-2'>
          <p className='login-box-msg p-0 mb-2 fs-4'>Authentication</p>
        </div>
        <div className='card-body login-card-body'>
          <p className='fs-8 text-center'>Use the code sent to your email to verify your identity.</p>
          <form onSubmit={handleVerify}>
            <div className='input-group mb-4'>
              <div className='form-floating'>
                <input
                  name='verificationCode'
                  id='verificationCode'
                  type='text'
                  className='form-control'
                  value={verificationCode}
                  onChange={(e) => setVerificationCode(e.target.value)}
                  placeholder=''
                  required
                />
                <label htmlFor='token'>Code</label>
              </div>
              <div className='input-group-text'>
                <span className='bi bi-upc'></span>
              </div>
            </div>
            <div className='row justify-content-center mb-4'>
              <div className='col-6'>
                <div className='d-grid gap-2'>
                  <button type='submit' className='btn btn-primary' disabled={isLoading}>
                    Confirm
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
  )
}

export default VerifyForm;