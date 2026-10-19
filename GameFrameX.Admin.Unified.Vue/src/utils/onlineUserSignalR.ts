import * as SignalR from '@microsoft/signalr';
import {clearAccessTokens, getToken} from './axiosUtils';
import {systemAuthLogoutPost} from '../api/systemAuth';

let connection: SignalR.HubConnection | null = null;
let started = false;

const ensureConnection = (): SignalR.HubConnection => {
	if (connection) return connection;

	connection = new SignalR.HubConnectionBuilder()
		.configureLogging(SignalR.LogLevel.Information)
		.withUrl(`${window.__env__.VITE_API_URL}/hubs/onlineUser?access_token=${getToken()}`, {
			transport: SignalR.HttpTransportType.WebSockets,
			skipNegotiation: true
		})
		.withAutomaticReconnect({
			nextRetryDelayInMilliseconds: () => {
				return 5000;
			},
		})
		.build();

	connection.keepAliveIntervalInMilliseconds = 15 * 1000;
	connection.serverTimeoutInMilliseconds = 30 * 60 * 1000;

	connection.onclose(async () => {
		console.debug('[signalR] connection closed');
	});
	connection.onreconnecting(() => {
		console.debug('[signalR] reconnecting');
	});
	connection.onreconnected(() => {
		console.debug('[signalR] reconnected');
	});
	connection.on('ForceOffline', async (data: any) => {
		console.debug('[signalR] force offline', data);
		await connection?.stop();
		started = false;

		await systemAuthLogoutPost();
		clearAccessTokens();
	});

	return connection;
};

const startConnection = async (): Promise<SignalR.HubConnection> => {
	const conn = ensureConnection();
	if (!started) {
		started = true;
		await conn.start();
		console.debug('[signalR] connection started');
	}
	return conn;
};

export {startConnection};
