function test-this ($p) {
	"Hello $p"
}

$b = 'aasdf'; 
$c = "aasdf"; 

$b = $true
switch -parallel ($b) {
	default {$_}
}