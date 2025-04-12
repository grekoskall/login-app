import { useEffect, useState } from 'react'
import NotificationComponent from '@components/react/NotificationComponent';

const EditUsersComponent = () => {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [telephone, setTelephone] = useState('');
  const [photo, setPhoto] = useState('default-150x150.png');
  const basePath = '../../../../dist/assets/img/'
  const [notificationMessage, setNotificationMessage] = useState("");
  const [notificationCount, setNotificationCount] = useState(0);
  const [notificationType, setNotificationType] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [cooldown, setCooldown] = useState(false);

  useEffect(async () => {
    const fetchUserDetails = async () => {
      try {
        const response = await fetch('https://localhost:5033/users/user-details', {
          method: 'GET',
          credentials: 'include',
        });

        if (!response.ok) {
          throw new Error('Failed to fetch user details');
        }

        const data = await response.json();

        setFirstName(data.firstName || '');
        setLastName(data.lastName || '');
        setTelephone(data.telephone || '');
        setPhoto(data.photoPath || 'default-150x150.png');
      } catch (error) {
        showNotification(`${error}`, "error");
      }
    };

    await fetchUserDetails();
  }, []);


  const showNotification = (message, type) => {
    setNotificationMessage(message);
    setNotificationType(type);
    setNotificationCount((prevKey) => prevKey + 1);
  };

  const handleUpdate = async (event) => {
    event.preventDefault();
    if (cooldown) return;
    setIsLoading(true);
    setCooldown(true);
    try {
      const batch = {
        firstName: firstName,
        lastName: lastName,
        telephone: telephone,
        photoPath: photo
      }

      const response = await fetch('https://localhost:5033/users/edit', {
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
        showNotification("User Details Updated", "success")
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
      <form onSubmit={handleUpdate} className="mt-3 mb-3 me-5 ms-5 ">
        <div className="row">
          <div className="col-12 col-md-9">
            <div className='form-row row'>
              <div className="form-group col-12 col-md-4">
                <label className='me-3' htmlFor="firstName">First Name: </label>
                <input
                  name='firstName'
                  id='firstName'
                  type='text'
                  value={firstName}
                  className='form-control'
                  onChange={(e) => setFirstName(e.target.value)}
                  placeholder=''
                  required
                />
              </div>
              <div className="form-group col-12 col-md-4">
                <label className='ms-3 me-3' htmlFor="lastName">Last Name: </label>
                <input
                  name='lastName'
                  id='lastName'
                  type='text'
                  value={lastName}
                  className='form-control'
                  onChange={(e) => setLastName(e.target.value)}
                  placeholder=''
                  required
                />
              </div>
            </div>
            <div className='form-row row mt-3'>
              <div className="form-group col-12 col-md-4">
                <label className='ms-3 me-3' htmlFor="telephone">Telephone:</label>
                <input
                  name='telephone'
                  id='telephone'
                  type='tel'
                  className='form-control'
                  value={telephone}
                  onChange={(e) => setTelephone(e.target.value)}
                  placeholder=''
                  required
                />
              </div>
            </div>
          </div>
          <div className="col-12 col-md-3">
            <img
              src={basePath + photo}
              className='rounded-circle shadow'
              alt='Profile Photo'
              style={{ width: "150px", height: "150px", marginTop: "10px" }}
            />
          </div>
        </div>
        <div className="form-row row mt-3 align-items-end">
          <div className="form-group col-12 col-md-4">
            <label className='ms-3 me-3' htmlFor="photo">Photo:</label>
            <select className='form-select my-1 mr-sm-2' id="photo"
              value={photo}
              onChange={(e) => setPhoto(e.target.value)}
            >
              <option value="default-150x150.png">Choose Photo...</option>
              <option value="user1-128x128.jpg">Default 1</option>
              <option value="user2-160x160.jpg">Default 2</option>
              <option value="user3-128x128.jpg">Default 3</option>
            </select>
          </div>
          <div className="form-group col-12 col-md-4 d-flex mt-2 justify-content-center">
            <button type="submit" className="btn btn-primary"
              disabled={isLoading}
            >
              {isLoading ? "Updating..." : "Update"}
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

export default EditUsersComponent;