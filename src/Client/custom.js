
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
    }
}