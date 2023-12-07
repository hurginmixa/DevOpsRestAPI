/// <reference path="FolderElementClass.ts" />
/// <reference path="LineMarkerClass.ts" />

let LineMarker: LineMarkerClass | null = null;

function onDocumentClick(ev: Event)
{
    if (!(ev.target instanceof HTMLTableCellElement)) 
    {
        return;
    }

    const cellElement: HTMLTableCellElement = ev.target as HTMLTableCellElement;
    const rowElement: HTMLTableRowElement = cellElement.parentElement as HTMLTableRowElement;

    let currentId : number = +rowElement.id;
    if (currentId <= 0)
    {
        return;
    }

    let oldIdNumber : number = (LineMarker?.ItemId ?? -1);
    LineMarker?.Hide();

    LineMarker = oldIdNumber === currentId ? null : new LineMarkerClass(rowElement);

    ev.cancelBubble = true;
}

function OnCollapseAll()
{
    let childList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_0`);
    for (let i = 0; i < childList.length; i++)
    {
        const element: HTMLElement = <HTMLElement>(childList[i]);
        FolderElementClass.CloseAllChilds(element.id);
    }
}

function OnMarkClick(markSpanElement: HTMLElement, ownerId: string) : boolean
{
    if (!(markSpanElement instanceof HTMLSpanElement))
    {
        return true;
    }

    let markSpan: FolderElementClass = new FolderElementClass(markSpanElement);

    if (markSpan.IsClose)
    {
        markSpan.Open();

        let subItemList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_${ownerId}`);
        for (let i = 0; i < subItemList.length; i++)
        {
            const element: HTMLElement = <HTMLElement>(subItemList[i]);
            element.style.display = "";
        }

        return false;
    }

    if (!markSpan.IsClose)
    {
        FolderElementClass.CloseAllChilds(ownerId);

        return false;
    }

    return true;
}
