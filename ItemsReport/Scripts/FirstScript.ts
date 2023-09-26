
function listenerFunction(ev: Event) 
{
    if (ev.srcElement instanceof HTMLTableCellElement) 
    {
        const cellElement: HTMLTableCellElement = ev.srcElement as HTMLTableCellElement;
        const rowElement: HTMLTableRowElement = cellElement.parentElement as HTMLTableRowElement;

        alert(`Ok2: !!! ${rowElement.id}`);
    }
}

//document.addEventListener("click", listenerFunction);

document.onclick = listenerFunction;
