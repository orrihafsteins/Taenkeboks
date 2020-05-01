class Card {
    constructor(title) {
        this.title = title;
        this.subtitle = 'SOME SUBTITLE';
        this.cardClass = 'info'
        this.items = {

        }
    }

    render() {
        function renderItem(key, value) {
            return `            
            <li class="list-group-item d-flex justify-content-between align-items-center">
                ${key}:
                <div>${value}</div>
            </li>`;
        }
        var items = Object.entries(this.items).map(function ([key, value], index) { return renderItem(key, value); }).join('\n')
        return `            
            <div class="bs-component">
                <div class="card text-white bg-${this.cardClass} mb-3">
                    <div class="card-header">${this.title}</div>
                    <div class="card-body">
                        <ul class="list-group">
                            ${items}
                        </ul>
                    </div>
                </div>
            </div>`;
    }
}

