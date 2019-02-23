import content from './public/docs/privacy-policy.md';
const React = require('react')
const ReactDOM = require('react-dom')
const ReactMarkdown = require('react-markdown')

export function wait_for_dom() {
    document.addEventListener("load", function() {
        console.log("DOM is ready");
        ReactDOM.render(
            <ReactMarkdown source={content} />,
            document.getElementById('pp-container')
        )
    });
}

