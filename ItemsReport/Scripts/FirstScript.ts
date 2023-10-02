const openItemCode : number = 128449;
const closeItemCode : number = 128448;

const openItemText = "\u{1F5C1}";
const closeItemText : string = "\u{1F5C0}";

//const openItemText = String.fromCharCode(openItemCode, 16);
alert(openItemText);


function OnCollapseAll()
{
    let childList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_0`);
    for (let i = 0; i < childList.length; i++)
    {
        const element: HTMLElement = <HTMLElement>(childList[i]);
        CloseAllChilds(element.id);
    }
}

function OnMarkClick(markDiv: HTMLElement, ownerId : string) : boolean
{
    if (!(markDiv instanceof HTMLSpanElement))
    {
        return true;
    }

    if (markDiv.innerText === closeItemText)
    {
        markDiv.innerText = openItemText;

        let childList: HTMLCollectionOf<Element> = document.getElementsByClassName(`childOf_${ownerId}`);
        for (let i = 0; i < childList.length; i++)
        {
            const element: HTMLElement = <HTMLElement>(childList[i]);
            element.style.display = "";
        }

        return false;
    }

    if (markDiv.innerText === openItemText)
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

    let markDiv = GetMarkSpanElement(document.getElementById(ownerId));
    if (markDiv != null)
    {
        markDiv.innerText = closeItemText;
    }
}

function GetMarkSpanElement(src: HTMLElement): HTMLElement
{
    let divList: HTMLCollectionOf<HTMLSpanElement> = src.getElementsByTagName("span");
    for (let i = 0; i < divList.length; i++)
    {
        const element: HTMLElement = divList[i];
        if (element.id === "mark")
        {
            return element;
        }
    }

    return null;
}
