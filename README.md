# StarNet

A Starbound multiplayer network. Think IRC, but for Starbound. The goal is to enable MMO-style gameplay
with thousands of players in the same universe.

**This is prerelease software, proceed at your own risk**

## Network

The StarNet network consists of a number of nodes, and a great deal more servers. Players connect to
these nodes, and the nodes connect them through the servers. The nodes communicate with each other
via UDP to keep tabs on the network, and proxy through to the servers. Each server can be a vanilla
Starbound server, but any other third party server should also work.

The idea is that any server operator can add their server to the network to make it easier for players
to find their servers and play on them. This also helps the StarNet network grow by adding capacity.
Each server would ideally host a star system or two, and StarNet would handle transparently moving
players between servers when they warp around.

## Installation

StarNet nodes are only designed to run on Linux, but you may have success with other platforms. On Linux,
make sure you have Mono 3.2.3 installed. **Debian stable has the wrong version of Mono**. The official
StarNet runs on Arch Linux. Anywho, get Mono and run this:

    $ git clone git://github.com/SirCmpwn/StarNet.git
    $ cd StarNet
    $ xbuild /p:Configuration=RELEASE
    $ cd StarNet/bin/Debug

Congrats, your binaries are in the working directory. You'll need to prepare a PostgreSQL database for
your network to use. Once you have a database prepared, your node needs an RSA keypair. Do this:

    $ openssl genrsa -out node.key
    $ openssl rsa -pubout -in node.key -out node.key.pub

This will generate your server's key in node.key, and the public key in node.key.pub. Keep the node.key
file secret. See [key compromise](https://github.com/SirCmpwn/StarNet/wiki/Key-Compromise) on the wiki
for help if it's lost. Next step:

    $ mono StarNet.exe

StarNet will walk you through the node setup. Keep all the defaults unless you know better. You'll have
to have your network's pSQL connection string prepared, something like
`Server=localhost;User ID=starnet;Password=password;Database=starnet;`. Once that's all done, all you
have left is to add the node to your network. If this is the first node in your network, you're already
done. Otherwise, you need to choose an existing node to vouch for your new node. We'll call this node
the "approver". Copy the approver's public key into the new node's host, then run this from the new node:

    $ mono StarNet.exe trust-key path/to/keyfile.key.pub

This will instruct your node to trust the other node when it invites you to the new network. Next, copy
the new node's public key to the approver's host, and run this from the approver's host:

    $ mono StarNet.exe add-node ip.ip.ip.ip port path/to/keyfile.pub

Fill in the details as appropriate. The new node should say something to the effect of
`Joined network 4313a38b-24c1-4f72-9f28-dbaf4be085e6`, and you're good to go! The new node should say
`Syncing with network...Done`, and once that "Done" appears, you can add it to your load balancer (a
DNS round robin should do the trick) and the new node will start accepting connections and doing its
thing.

## Contributing

If you'd like to contribute, please fork the repository and submit a pull request. If you're looking for
something to do, check out the [issues](https://github.com/SirCmpwn/StarNet/issues) on GitHub and comment
on whatever you intend to deal with. Make sure you adhere to coding styles already in use throughout the
project. Feel free to join us on IRC -
[##starbound-dev on irc.freenode.net](http://webchat.freenode.net/?channels=##starbound-dev) - if you
have any questions, or just want to chat.
