import { useEffect, useState } from 'react'
import NotificationComponent from '@components/react/NotificationComponent';

const UserMenuDropdown = () => {
  const [notificationMessage, setNotificationMessage] = useState("");
  const [notificationCount, setNotificationCount] = useState(0);
  const [notificationType, setNotificationType] = useState("");
  const [userData, setUserData] = useState(null);
  const [loading, setLoading] = useState(false);

  const basePath = '../../../../../dist/assets/img/'

  const showNotification = (message, type) => {
    setNotificationMessage(message);
    setNotificationType(type);
    setNotificationCount((prevKey) => prevKey + 1);
  };

  useEffect(() => {
    if (userData != null) return;

    setLoading(true);
    fetch('https://localhost:5033/users/user-details', {
      method: "GET",
      credentials: 'include',
      headers: {
        "Content-Type": "application/json"
      }
    }).then((res) => {
      if (res.status === 401) {
        showNotification("Unauthorized", "error");
      }
      return res.json();
    }).then(setUserData)
      .catch(() => {
        showNotification("Error fetching user data.", "error");
      }).finally(() => [
        setLoading(false)
      ])
  }, []);

  const handleLogout = async (event) => {
    event.preventDefault();
    try {

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
    } catch (err) {
      showNotification("An error occured.", "error");
    }
  }

  return (
    <div>
      <ul className='navbar-nav ms-auto'>
        <li className='nav-item'>
          <a className='nav-link' href='#' data-lte-toggle='fullscreen'>
            <i data-lte-icon='maximize' className='bi bi-arrows-fullscreen'></i>
            <i
              data-lte-icon='minimize'
              className='bi bi-fullscreen-exit'
              style={{ display: 'none' }}
            >
            </i>
          </a>
        </li>

        <li className='nav-item dropdown user-menu'>
          {
            userData ?
              <>
                <a href='#' className='nav-link dropdown-toggle' data-bs-toggle='dropdown'>
                  <img
                    src={basePath + userData.photoPath}
                    className='user-image rounded-circle shadow'
                    alt='User Image'
                  />
                  <span className='d-none d-md-inline'>{userData.firstName} {userData.lastName}</span>
                </a>
              </>
              : <></>
          }
          <ul className='dropdown-menu dropdown-menu-lg dropdown-menu-end'>
            <li className='user-header text-bg-primary'>
              {
                !loading && userData ?
                  <>
                    <img
                      src={basePath + userData.photoPath}
                      className='rounded-circle shadow'
                      alt='Profile Photo'
                    />
                    <p>
                      {userData.firstName} {userData.lastName}
                    </p>
                    <div
                      className='d-flex flex-column'
                    >
                      <small>
                        Email: {userData.email}
                      </small>
                      <small>
                        Telephone: {userData.telephone}
                      </small>
                    </div>
                  </>
                  : <></>
              }
            </li>

            <li className='user-footer'>
              <a href='#' onClick={handleLogout} className='btn btn-default btn-flat float-end'>Sign out</a>
            </li>
          </ul>
        </li>
      </ul >
      {/* Notification Component */}
      {
        notificationMessage &&
        <NotificationComponent
          client:load
          message={notificationMessage}
          type={notificationType}
          key={notificationCount} />
      }
    </div >
  )
}

export default UserMenuDropdown;