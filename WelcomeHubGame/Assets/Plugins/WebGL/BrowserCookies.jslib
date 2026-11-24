mergeInto(LibraryManager.library, {
  BrowserCookies_Set: function (keyPtr, valuePtr, days, pathPtr) {
    var key = UTF8ToString(keyPtr);
    var value = UTF8ToString(valuePtr);
    var path = UTF8ToString(pathPtr);

    var expires = "";
    if (days > 0) {
      var date = new Date();
      date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
      expires = "; expires=" + date.toUTCString();
    } else if (days < 0) {
      expires = "; expires=Thu, 01 Jan 1970 00:00:00 GMT";
    }

    var secureFlag = window.location.protocol === "https:" ? "; Secure" : "";
    document.cookie =
      encodeURIComponent(key) +
      "=" +
      encodeURIComponent(value) +
      expires +
      "; path=" +
      (path || "/") +
      "; SameSite=Lax" +
      secureFlag;
  },

  BrowserCookies_Get: function (keyPtr) {
    var targetKey = encodeURIComponent(UTF8ToString(keyPtr)) + "=";
    var cookies = document.cookie.split(";");
    var result = "";

    for (var i = 0; i < cookies.length; i++) {
      var cookie = cookies[i].trim();
      if (cookie.indexOf(targetKey) === 0) {
        try {
          result = decodeURIComponent(cookie.substring(targetKey.length));
        } catch (e) {
          result = cookie.substring(targetKey.length);
        }
        break;
      }
    }

    var bufferSize = lengthBytesUTF8(result) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(result, buffer, bufferSize);
    return buffer;
  }
});