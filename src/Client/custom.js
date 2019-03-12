
var sub_count_for_tutor = 0
const initLayoutContainer = require('opentok-layout-js');
const options = {
    maxRatio: 3/2,          // The narrowest ratio that will be used (default 2x3)
    minRatio: 9/16,         // The widest ratio that will be used (default 16x9)
    fixedRatio: false,      // If this is true then the aspect ratio of the video is maintained and minRatio and maxRatio are ignored (default false)
    bigClass: "OT_big",     // The class to add to elements that should be sized bigger
    bigPercentage: 0.8,      // The maximum percentage of space the big ones should take up
    bigFixedRatio: false,   // fixedRatio for the big ones
    bigMaxRatio: 3/2,       // The narrowest ratio to use for the big elements (default 2x3)
    bigMinRatio: 9/16,      // The widest ratio to use for the big elements (default 16x9)
    bigFirst: true,         // Whether to place the big one in the top left (true) or bottom right
    animate: true           // Whether you want to animate the transitions
};

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
    sub_count_for_tutor = 0;
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
        if (sub_count_for_tutor < 2) {
            var sub = session.subscribe(event.stream, 'layoutContainer', {
                insertMode: 'append',
                preferedResolution: { width: w, height: h }
                // width: '100%',
                // height: '100%'
            }, handle_error);
            sub_count_for_tutor++;

            var layoutContainer = document.getElementById("layoutContainer");
            // Initialize the layout container and get a reference to the layout method
            var layout = initLayoutContainer(layoutContainer).layout;
            layout();
        } else {
            console.log("Not enough room in the class!!");
        }
    });
    session.on('streamDestroyed', function (event) {
        sub_count_for_tutor--;
        var layoutContainer = document.getElementById("layoutContainer");
        // Initialize the layout container and get a reference to the layout method
        var layout = initLayoutContainer(layoutContainer).layout;
        layout();
        layout();
        if (sub_count_for_tutor < 0) {
            sub_count_for_tutor = 0
        }
    });
}
export function init_session(key, session_id) {
    var session = OT.initSession(key, session_id);
    return session;
}