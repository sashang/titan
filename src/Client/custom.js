
function handle_error(error) {
    if (error) {
        alert(error.message);
        console.log(error.message);
    }
}

export function connect(session, token) {
    if (session)
        session.connect(token, handle_error);
}
export function disconnect(session) {
    console.log("disconnecting session");
    if (session) {
        session.disconnect();
    }
}
export function init_pub(div_id, res, email) {
    // Create a publisher
    var publisher = OT.initPublisher(div_id, {
        insertMode: 'append',
        resolution: res,
        width: '100%',
        height: '100%',
        name: email
    }, function (error) {
        if (error) {
            alert(error.message);
            console.log(error.message);
        }
        else {
            console.log("Publisher initialized.");
        }
    });
    return publisher;
}
export function connect_session_with_pub(session, publisher, token) {
    // Connect to the session
    session.connect(token, function (error) {
        // If the connection is successful, publish to the session
        if (error) {
            alert(error.message);
            console.log(error.message);
        }
        else {
            session.publish(publisher, function (error) {
                handle_error(error);
            });
        }
    });
}
export function on_streamcreate_subscribe_filter(session, w, h, email) {
    session.on('streamCreated', function (event) {
        if (event.stream.name == email) {
            session.subscribe(event.stream, 'subscriber', {
                insertMode: 'append',
                preferedResolution: { width: w, height: h },
                width: '100%',
                height: '100%'
            }, handle_error);
        }
    });
}
export function on_streamcreate_subscribe(session, w, h) {
    session.on('streamCreated', function (event) {
        session.subscribe(event.stream, 'subscriber', {
            insertMode: 'append',
            preferedResolution: { width: w, height: h },
            width: '100%',
            height: '100%'
        }, handle_error);
    });
}
export function connect_session_with_sub(session, token) {
    // Connect to the session
    session.connect(token, function (error) {
        // If the connection is successful, publish to the session
        if (error) {
            alert(error.message);
            console.log(error.message);
        }
        else {
            session.on('streamCreated', function (event) {
                session.subscribe(event.stream, 'subscriber', {
                    insertMode: 'append',
                    width: '100%',
                    height: '100%'
                }, handle_error);
            });
        }
    });
}
export function init_session(key, session_id) {
    var session = OT.initSession(key, session_id);
    return session;
}