import React from 'react';

class TutorSubscriber extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            session: OT.initSession(this.props.apiKey, this.props.session),
        }
    }

    componentDidMount() {
        if (this.state.session !== null) {
            this.state.session.connect(this.props.token, (err) => {
                this.state.session.on('streamCreated', (event) => {
                    this.state.session.subscribe(event.stream, 'subscriber', {
                            insertMode: 'append',
                            width: '100%',
                            height: '100%'
                        });
                    });
                });
            }
    }

    componentWillUnmount() {
        this.state.session.disconnect();
    }


    render() {
        return (
            <div id="subscriber">
            </div>
        )
    }
}

export default TutorSubscriber;