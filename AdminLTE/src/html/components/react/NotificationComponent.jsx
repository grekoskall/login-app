import React, { useRef, useEffect } from "react";

const NotificationComponent = ({ message, type }) => {
  const messageBoxRef = useRef(null);

  useEffect(() => {
    if (message) {
      const messageBox = new window.bootstrap.Toast(messageBoxRef.current);
      messageBox.show();
    }
  }, [message]); // Runs when `message` updates

  return (
    <div>
      {/* MessageBox Container */}
      <div className="toast-container position-fixed bottom-0 end-0 p-3">
        <div
          ref={messageBoxRef}
          className={`toast align-items-center border-0
             ${type === 'success' ? 'text-bg-success' : type === 'error' ? 'text-bg-danger' : 'text-bg-warning'} 
             `}
          role="alert"
          aria-live="assertive"
          aria-atomic="true"
        >
          <div className="d-flex">
            <div className="toast-body">{message}</div>
            <button
              type="button"
              className="btn-close btn-close-white me-2 m-auto"
              data-bs-dismiss="toast"
              aria-label="Close"
            ></button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default NotificationComponent;
