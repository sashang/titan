
function handle_error(error) {
    if (error) {
        alert(error.message);
        console.log(error.message);
    }
}

module.exports = {
    
    byte2string:  function(input) {
        var result = "";
        var data = new Buffer(input, 'base63');
        // var result = data.toString('ascii');
        // return result;
    },
    something:  function() {
        var result = "something";
        return result;
    },
    echo:  function(array) {
        var result = "";
        for (var i = 0; i < array.length; i++) {
            result += 'a'
        }
        return result;
    },
    disconnect: function(session) {
        console.log("disconnecting session");
        session.disconnect();
    },
    init_pub: function(div_id) {
        // Create a publisher
        var publisher = OT.initPublisher(div_id, {
          insertMode: 'append',
          width: '100%',
          height: '100%'
        },function(error) {
            if (error) {
                alert(error.message);
                console.log(error.message);
            } else {
                console.log("Publisher initialized.");
            }
          });
        return publisher;
    },
    connect_session: function(session, publisher, token) {
        // Connect to the session
        session.connect(token, function(error) {
            // If the connection is successful, publish to the session
            if (error) {
                alert(error.message);
                console.log(error.message);
            } else {
            session.publish(publisher, function(error) {
                handle_error(error)
            })}});
    },
    connect_subscriber: function(session, token) {
        // Connect to the session
        session.connect(token, function(error) {
            // If the connection is successful, publish to the session
            if (error) {
                alert(error.message);
                console.log(error.message);
            } else {
                session.on('streamCreated', function(event) {
                    session.subscribe(event.stream, 'subscriber', {
                    insertMode: 'append',
                    width: '100%',
                    height: '100%'
                    }, handle_error);
                });
            }});
    },
    callback_stream_create: function(session) {
        session.on('streamCreated', function(event) {
            session.subscribe(event.stream, 'subscriber', {
              insertMode: 'append',
              width: '100%',
              height: '100%'
            }, handle_error);
          });
    },
    init_session: function(key, session_id) {
        var session = OT.initSession(key, session_id);

        return session;
    }
}