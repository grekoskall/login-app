import { useState } from 'react'
import NotificationComponent from '@components/react/NotificationComponent';

const CreateUserForm = () => {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [telephone, setTelephone] = useState('');
  const [email, setEmail] = useState('');
  const [photo, setPhoto] = useState('default-150x150.png');
  const [password, setPassword] = useState('');
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
  const handleCreate = async (event) => {
    event.preventDefault();
    if (cooldown) return;
    setIsLoading(true);
    setCooldown(true);
    try {
      const batch = {
        firstName: firstName,
        lastName: lastName,
        telephone: telephone,
        email: email,
        photoPath: photo,
        password: password
      }

      const response = await fetch('https://localhost:5033/users/create', {
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
        showNotification("User Creation Success", "success")
      }
    } catch (error) {
      showNotification("Error during creation.", "error");
    } finally {
      setIsLoading(false);
      setTimeout(() => setCooldown(false), 1500);
    }

  }

  return (
    <>
      <form onSubmit={handleCreate} className="mt-3 mb-3 me-5 ms-5 ">
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
              <div className='form-group col-12 col-md-4'>
                <label className='me-3' htmlFor="email">Email: </label>
                <input
                  name='email'
                  id='email'
                  type='email'
                  value={email}
                  className='form-control'
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder=''
                  required
                />
              </div>
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
          <div className='form-group col-12 col-md-4'>
            <label className='me-3' htmlFor="password">Password: </label>
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
          <div className="form-group col-12 col-md-4">
            <label className='ms-3 me-3' htmlFor="photo">Photo:</label>
            <select className='form-select my-1 mr-sm-2' id="photo"
              value={photo}
              onChange={(e) => setPhoto(e.target.value)} // Update state correctly
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
              {isLoading ? "Creating..." : "Create"}
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

export default CreateUserForm;