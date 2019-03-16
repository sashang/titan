import React from 'react';

class TutorPublisher extends React.Component {
    constructor(props) {
        super(props);
    }

    componentDidMount() {
        this.props.session.connect(this.props.token, (err) => {
            if (err) {
                console.log(error.message);
            } else {
                var pub = OT.initPublisher("publisher", {
                    insertMode: 'append',
                    width: '100%',
                    height: '100%',
                    name: this.props.tutorEmail });
                this.state.session.publish(pub);
            }
        });
    }

    componentWillUnmount() {
        this.state.session.disconnect();
    }

    render() {
        return (
            <div id="publisher">
            </div>
        )
    }
}

export default TutorPublisher;