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

    getFormattedDate(messageDate) {

        let dt = new Date(messageDate);
        return dt.toLocaleString("ru", {
            year: '2-digit', month: '2-digit', day: '2-digit',
            hour: '2-digit', minute: '2-digit', second: '2-digit'
        });
    }

    render() {

        return <div>

            {this.state.messages.map(message =>
                <p>
                    <table width="100%">
                        <tbody>
                            <tr>
                                <td width="50%">
                                    <b>{message.author.name}</b>
                                    &nbsp;&nbsp;&nbsp;
                                    <img title={message.author.userRole.name} height="20" src={ message.author.userRole.imageAddress} />
                                </td>
                                <td width="50%" align="right">{this.getFormattedDate(message.messageDate)}</td>
                            </tr>
                        </tbody>
                    </table>
                    <p>{message.text}</p>
                </p>
            )}
        </div>;
    }
}

ReactDOM.render(
    <MessagesList />,
    document.getElementById("chatroom")
);