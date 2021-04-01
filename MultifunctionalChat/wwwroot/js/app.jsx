class MessagesList extends React.Component {

    constructor(props) {
        super(props);
        this.state = { messages: [] };
    }

    // загрузка данных
    loadData() {
        var xhr = new XMLHttpRequest();
        xhr.open("get", "/message", true);

        xhr.onload = function () {
            var data = JSON.parse(xhr.responseText);
            this.setState({
                messages: data
            });
        }.bind(this);
        xhr.send();
    }

    componentDidMount() {
        this.loadData();
    }
    
    render() {

        return <div>

            <h3>История сообщений</h3>
            {this.state.messages.reverse().map(message =>
                <p>
                    <b>{message.userName}</b>
                    <p>{message.text}</p>
                </p>
            )}
        </div>;
    }
}

ReactDOM.render(
    <MessagesList />,
    document.getElementById("content")
);