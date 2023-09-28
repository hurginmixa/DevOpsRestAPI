﻿
function CollapseAll() {
    let childList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_0`);
    for (let i = 0; i < childList.length; i++) {
        const element: HTMLElement = <HTMLElement>(childList[i]);
        CloseAllChilds(element.id);
    }
}

function onDocumentClick(ev: Event) {
    if (!(ev.target instanceof HTMLTableCellElement)) {
        return;
    }

    const cellElement: HTMLTableCellElement = ev.target as HTMLTableCellElement;
    const rowElement: HTMLTableRowElement = cellElement.parentElement as HTMLTableRowElement;

    let markDiv : HTMLElement = GetMarkSpanElement(rowElement);

    if (markDiv?.innerText === "[+]") 
    {
        markDiv.innerText = "[-]";

        let childList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_${rowElement.id}`);
        for (let i = 0; i < childList.length; i++) {
            const element: HTMLElement = <HTMLElement>(childList[i]);
            element.style.display = "";
        }

        return;
    }

    if (markDiv?.innerText === "[-]") {
        CloseAllChilds(rowElement.id);

        return;
    }
}

function CloseAllChilds(ownerId: string) {
    let childList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_${ownerId}`);

    if (childList.length === 0) {
        return;
    }

    for (var i = 0; i < childList.length; i++) {
        const child: HTMLElement = <HTMLElement>(childList[i]);
        CloseAllChilds(child.id);

        child.style.display = "none";
    }

    let markDiv =  GetMarkSpanElement(document.getElementById(ownerId));
    if (markDiv != null) {
        markDiv.innerText = "[+]";
    }
}

function GetMarkSpanElement(src : HTMLElement) : HTMLElement {
    let divList: HTMLCollectionOf<HTMLSpanElement> = src.getElementsByTagName("span");
    for (let i = 0; i < divList.length; i++) {
        const element: HTMLElement = divList[i];
        if (element.id === "mark") {
            return element;
        }
    }

    return null;
}

document.ondblclick = onDocumentClick;
