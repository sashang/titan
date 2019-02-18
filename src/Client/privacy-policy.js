import htmlContent from '/home/sashan/code/titan/src/Client/public/docs/privacy-policy.html';
import React from 'react';
var createReactClass = require('create-react-class');

const PrivacyPolicy = createReactClass({
    render() {
        return ( <div> dangerouslySetInnerHTML={ {__html: htmlContent} } </div>);
    }
});
export default PrivacyPolicy;