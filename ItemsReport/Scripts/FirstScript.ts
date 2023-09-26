
function listenerFunction(ev: Event) 
{
    if (ev != null) 
    {
        alert("Ok2: !!! " + ev.srcElement);
    }

    var a : string = "Mixa11122333444555";

    alert(a);
}

//document.addEventListener("click", listenerFunction);

document.onclick = listenerFunction;
