
function listenerFunction(ev: Event) 
{
    ev = "mixa";

    alert("Ok2" + ev.srcElement);
}

//document.addEventListener("click", listenerFunction);

document.onclick = listenerFunction;
