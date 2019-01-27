
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
    handleError: function(error) {
        if (error) {
          alert(error.message);
        }
    },
    initialize_session: function(key, session_id, token) {
        // Subscribe to a newly created stream
        var session = OT.initSession(key, session_id);
      
        // Create a publisher
        var publisher = OT.initPublisher('publisher', {
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
      
        // Connect to the session
        session.connect(token, function(error) {
          // If the connection is successful, publish to the session
          if (error) {
            alert(error.message);
            console.log(error.message);
          } else {
            session.publish(publisher, function(error) {
                if (error) {
                    alert(error.message);
                    console.log(error.message);
                } else {
                    console.log("Published session");
                }
            });
          }
        });
    }

}