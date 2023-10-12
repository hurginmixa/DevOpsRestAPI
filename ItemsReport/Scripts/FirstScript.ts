class MarkSpanObject
{
    private openItem = "\u25e2";
    private closeItem = "\u25b7";
    
    private _markSpanElement : HTMLSpanElement;

    constructor(markSpanElement : HTMLSpanElement)
    {
        this._markSpanElement = markSpanElement;
    }

    get IsClose()
    {
        return this._markSpanElement.innerText === this.closeItem;
    }

    Open()
    {
        this._markSpanElement.innerText = this.openItem;
    }

    Close()
    {
        this._markSpanElement.innerText = this.closeItem;
    }

    static GetMarkSpanElement(src: HTMLElement | null): (MarkSpanObject | null)    
    {
        if (src === null)
        {
            return null;
        }

        let spanList: HTMLCollectionOf<HTMLSpanElement> = src.getElementsByTagName("span");
        for (let i = 0; i < spanList.length; i++)
        {
            const element: HTMLElement = spanList[i];
            if (element.id === "mark")
            {
                return new MarkSpanObject(element);
            }
        }

        return null;
    }
}

function OnCollapseAll()
{
    let childList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_0`);
    for (let i = 0; i < childList.length; i++)
    {
        const element: HTMLElement = <HTMLElement>(childList[i]);
        CloseAllChilds(element.id);
    }
}

function OnMarkClick(markSpanElement: HTMLElement, ownerId : string) : boolean
{
    if (!(markSpanElement instanceof HTMLSpanElement))
    {
        return true;
    }

    const markSpan : MarkSpanObject = new MarkSpanObject(markSpanElement);

    if (markSpan.IsClose)
    {
        markSpan.Open();

        let childList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_${ownerId}`);
        for (let i = 0; i < childList.length; i++)
        {
            const element: HTMLElement = <HTMLElement>(childList[i]);
            element.style.display = "";
        }

        return false;
    }

    if (!markSpan.IsClose)
    {
        CloseAllChilds(ownerId);

        return false;
    }

    return true;
}

function CloseAllChilds(ownerId: string)
{
    let childList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_${ownerId}`);

    if (childList.length === 0)
    {
        return;
    }

    for (let i = 0; i < childList.length; i++)
    {
        const child: HTMLElement = <HTMLElement>(childList[i]);
        CloseAllChilds(child.id);

        child.style.display = "none";
    }

    MarkSpanObject.GetMarkSpanElement(document.getElementById(ownerId))?.Close();
}

class LineMarkerClass
{
    private _rowElement: HTMLTableRowElement;
    private _backgroundColor: string;
    private _color: string;
    private _anchorColor: string;

    private  constructor(rowElement: HTMLTableRowElement)
    {
        this._rowElement = rowElement;
        this._backgroundColor =  this._rowElement.style.backgroundColor;
        this._color = this._rowElement.style.color;

        this._rowElement.style.backgroundColor = "#707b7c";
        this._rowElement.style.color = "#fdfefe";

        let anchorList : HTMLCollectionOf<HTMLAnchorElement> = this._rowElement.getElementsByTagName("a");
        if (anchorList.length > 0)
        {
            this._anchorColor = anchorList[0].style.color;

            for (let i = 0; i < anchorList.length; i++)
            {
                anchorList[i].style.color = "#fdfefe";
            }
        }
    }

    public Hide() : void
    {
        this._rowElement.style.backgroundColor = this._backgroundColor;
        this._rowElement.style.color = this._color;

        let anchorList : HTMLCollectionOf<HTMLAnchorElement> = this._rowElement.getElementsByTagName("a");
        if (anchorList.length > 0)
        {
            for (let i = 0; i < anchorList.length; i++)
            {
                anchorList[i].style.color = this._anchorColor;
            }
        }
    }

    public static GetLineMarker(target: EventTarget) : LineMarkerClass | null
    {
        if (!(target instanceof HTMLTableCellElement)) {
            return null;
        }

        const cellElement: HTMLTableCellElement = target as HTMLTableCellElement;
        const rowElement: HTMLTableRowElement = cellElement.parentElement as HTMLTableRowElement;

        return new LineMarkerClass(rowElement);
    }
}

let LineMarker: LineMarkerClass = null;

function onDocumentClick(ev: Event)
{
    LineMarker?.Hide();

    LineMarker = LineMarkerClass.GetLineMarker(ev.target);
}

document.onclick = onDocumentClick;
