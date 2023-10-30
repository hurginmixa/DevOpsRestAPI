//
// class MarkSpanObject
//

class MarkSpanObject
{
    private openItem : string = "\u25e2";
    private closeItem : string = "\u25b7";
    
    private readonly _markSpanElement : HTMLSpanElement;

    public constructor(markSpanElement : HTMLSpanElement)
    {
        this._markSpanElement = markSpanElement;
    }

    public get IsClose()
    {
        return this._markSpanElement.innerText === this.closeItem;
    }

    public Open()
    {
        this._markSpanElement.innerText = this.openItem;
    }

    public Close()
    {
        this._markSpanElement.innerText = this.closeItem;
    }

    public static GetMarkSpanElement(src: HTMLElement | null): (MarkSpanObject | null)    
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

//
// class LineMarkerClass
//

class LineMarkerClass
{
    private readonly savedRowElement: HTMLTableRowElement;
    private readonly savedBackgroundColor: string;
    private readonly savedTextColor: string;
    private readonly savedAnchorColor: string;

    public constructor(rowElement: HTMLTableRowElement)
    {
        this.savedRowElement = rowElement;
        this.savedBackgroundColor =  this.savedRowElement.style.backgroundColor;
        this.savedTextColor = this.savedRowElement.style.color;

        this.savedRowElement.style.backgroundColor = "#707b7c";
        this.savedRowElement.style.color = "#fdfefe";

        let anchorList : HTMLCollectionOf<HTMLAnchorElement> = this.savedRowElement.getElementsByTagName("a");
        if (anchorList.length > 0)
        {
            this.savedAnchorColor = anchorList[0].style.color;

            for (let i = 0; i < anchorList.length; i++)
            {
                anchorList[i].style.color = "#fdfefe";
            }
        }
    }

    public get ItemId() : number
    {
        return +this.savedRowElement.id;
    }

    public Hide() : void
    {
        this.savedRowElement.style.backgroundColor = this.savedBackgroundColor;
        this.savedRowElement.style.color = this.savedTextColor;

        let anchorList : HTMLCollectionOf<HTMLAnchorElement> = this.savedRowElement.getElementsByTagName("a");
        if (anchorList.length > 0)
        {
            for (let i = 0; i < anchorList.length; i++)
            {
                anchorList[i].style.color = this.savedAnchorColor;
            }
        }
    }
}

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
    LineMarker = null;

    if (oldIdNumber === currentId)
    {
        return;
    }

    LineMarker = new LineMarkerClass(rowElement);

    ev.cancelBubble = true;
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

function OnMarkClick(markSpanElement: HTMLElement, ownerId: string) : boolean
{
    if (!(markSpanElement instanceof HTMLSpanElement))
    {
        return true;
    }

    let markSpan: MarkSpanObject = new MarkSpanObject(markSpanElement);

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
        let child: HTMLElement = <HTMLElement>(childList[i]);
        CloseAllChilds(child.id);

        child.style.display = "none";
    }

    MarkSpanObject.GetMarkSpanElement(document.getElementById(ownerId))?.Close();
}
